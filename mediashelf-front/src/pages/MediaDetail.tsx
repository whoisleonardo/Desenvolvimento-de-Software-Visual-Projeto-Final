import { useEffect, useState, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { mediaApi, reviewApi } from '@/services/api';
import { useAuth } from '@/contexts/AuthContext';
import type { Media } from '@/types';
import { Header } from '@/components/Header';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { useToast } from '@/hooks/use-toast';
import { Star, Trash2 } from 'lucide-react';

export default function MediaDetail() {
  const { id } = useParams<{ id: string }>();
  const { user } = useAuth();
  const navigate = useNavigate();
  const { toast } = useToast();
  const [media, setMedia] = useState<Media | null>(null);
  const [loading, setLoading] = useState(true);
  const [rating, setRating] = useState(5);
  const [comment, setComment] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const loadMedia = useCallback(async () => {
    try {
      const data = await mediaApi.getById(Number(id));
      setMedia(data);
    } catch (error) {
      toast({
        title: 'Erro',
        description: 'Erro ao carregar mídia',
        variant: 'destructive',
      });
    } finally {
      setLoading(false);
    }
  }, [id, toast]);

  useEffect(() => {
    if (id) {
      loadMedia();
    }
  }, [id, loadMedia]);

  const handleSubmitReview = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!user || !media) return;

    setSubmitting(true);
    try {
      await reviewApi.create({
        rating,
        comment,
        userId: user.id,
        mediaId: media.id,
      });
      toast({
        title: 'Sucesso!',
        description: 'Avaliação criada com sucesso',
      });
      setComment('');
      setRating(5);
      loadMedia();
    } catch (error) {
      toast({
        title: 'Erro',
        description: error instanceof Error ? error.message : 'Erro ao criar avaliação',
        variant: 'destructive',
      });
    } finally {
      setSubmitting(false);
    }
  };

  const handleDeleteMedia = async () => {
    if (!media || !user) return;
    if (media.userId !== user.id) {
      toast({
        title: 'Erro',
        description: 'Você não pode deletar esta mídia',
        variant: 'destructive',
      });
      return;
    }

    try {
      await mediaApi.delete(media.id);
      toast({
        title: 'Sucesso!',
        description: 'Mídia deletada com sucesso',
      });
      navigate('/');
    } catch (error) {
      toast({
        title: 'Erro',
        description: 'Erro ao deletar mídia',
        variant: 'destructive',
      });
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-background">
        <Header />
        <main className="container mx-auto px-4 py-8">
          <div className="text-center text-muted-foreground">Carregando...</div>
        </main>
      </div>
    );
  }

  if (!media) {
    return (
      <div className="min-h-screen bg-background">
        <Header />
        <main className="container mx-auto px-4 py-8">
          <div className="text-center text-muted-foreground">Mídia não encontrada</div>
        </main>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-background">
      <Header />
      <main className="container mx-auto px-4 py-8">
        <div className="mx-auto max-w-4xl space-y-8">
          <Card>
            <CardHeader>
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <CardTitle className="text-3xl">{media.title}</CardTitle>
                  <CardDescription className="mt-2">
                    Por {media.user?.name} • {new Date(media.createdAt).toLocaleDateString('pt-BR')}
                  </CardDescription>
                </div>
                {user && media.userId === user.id && (
                  <Button variant="destructive" size="icon" onClick={handleDeleteMedia}>
                    <Trash2 className="h-4 w-4" />
                  </Button>
                )}
              </div>
              {media.averageRating !== undefined && media.averageRating > 0 && (
                <div className="mt-4 flex items-center gap-2">
                  <Badge variant="secondary" className="flex items-center gap-1 text-base">
                    <Star className="h-4 w-4 fill-current" />
                    {media.averageRating.toFixed(1)}
                  </Badge>
                  <span className="text-sm text-muted-foreground">
                    {media.totalReviews} {media.totalReviews === 1 ? 'avaliação' : 'avaliações'}
                  </span>
                </div>
              )}
            </CardHeader>
            <CardContent>
              <p className="text-muted-foreground">{media.description}</p>
            </CardContent>
          </Card>

          {user && (
            <Card>
              <CardHeader>
                <CardTitle>Deixe sua Avaliação</CardTitle>
              </CardHeader>
              <CardContent>
                <form onSubmit={handleSubmitReview} className="space-y-4">
                  <div className="space-y-2">
                    <Label>Nota (0-5)</Label>
                    <div className="flex items-center gap-2">
                      {[1, 2, 3, 4, 5].map((value) => (
                        <button
                          key={value}
                          type="button"
                          onClick={() => setRating(value)}
                          className="transition-colors"
                        >
                          <Star
                            className={`h-8 w-8 ${
                              value <= rating ? 'fill-primary text-primary' : 'text-muted-foreground'
                            }`}
                          />
                        </button>
                      ))}
                      <span className="ml-2 text-sm text-muted-foreground">{rating}/5</span>
                    </div>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="comment">Comentário</Label>
                    <Textarea
                      id="comment"
                      placeholder="Escreva sua opinião sobre esta mídia..."
                      value={comment}
                      onChange={(e) => setComment(e.target.value)}
                      required
                      maxLength={1000}
                      rows={4}
                    />
                  </div>
                  <Button type="submit" disabled={submitting}>
                    {submitting ? 'Enviando...' : 'Enviar Avaliação'}
                  </Button>
                </form>
              </CardContent>
            </Card>
          )}

          <Card>
            <CardHeader>
              <CardTitle>Avaliações</CardTitle>
            </CardHeader>
            <CardContent>
              {media.reviews && media.reviews.length > 0 ? (
                <div className="space-y-4">
                  {media.reviews.map((review) => (
                    <div key={review.id} className="border-b pb-4 last:border-0">
                      <div className="mb-2 flex items-center justify-between">
                        <div className="flex items-center gap-2">
                          <span className="font-semibold">{review.user?.name}</span>
                          <Badge variant="secondary" className="flex items-center gap-1">
                            <Star className="h-3 w-3 fill-current" />
                            {review.rating}
                          </Badge>
                        </div>
                        <span className="text-xs text-muted-foreground">
                          {new Date(review.createdAt).toLocaleDateString('pt-BR')}
                        </span>
                      </div>
                      <p className="text-sm text-muted-foreground">{review.comment}</p>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="text-center text-muted-foreground">Nenhuma avaliação ainda</p>
              )}
            </CardContent>
          </Card>
        </div>
      </main>
    </div>
  );
}
